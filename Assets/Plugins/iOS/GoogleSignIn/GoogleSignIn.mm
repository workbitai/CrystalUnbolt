    //
    // GoogleSignIn.mm — v6+/v7 compatible shim for Unity plugin 1.x
    //

    #if __has_include(<GoogleSignIn/GoogleSignIn.h>)
      #import <GoogleSignIn/GoogleSignIn.h>
    #else
      #import <GoogleSignIn/GIDSignIn.h>
    #endif
    #import <UnityAppController.h>
    #import <objc/message.h>

    // ===== Status codes expected by Unity side =====
    static const int kStatusCodeSuccessCached   = -1;
    static const int kStatusCodeSuccess         = 0;
    static const int kStatusCodeApiNotConnected = 1;
    static const int kStatusCodeCanceled        = 2;
    static const int kStatusCodeInterrupted     = 3;
    static const int kStatusCodeInvalidAccount  = 4;
    static const int kStatusCodeTimeout         = 5;
    static const int kStatusCodeDeveloperError  = 6;
    static const int kStatusCodeInternalError   = 7;
    static const int kStatusCodeNetworkError    = 8;
    static const int kStatusCodeError           = 9;

    // ===== Utility: pause/resume Unity when showing UI =====
    static inline void UnpauseUnityPlayer() {
      dispatch_async(dispatch_get_main_queue(), ^{
        if (UnityIsPaused() > 0) {
          UnityPause(0);
        }
      });
    }

    // ===== Pending operation result (shared with C API) =====
    typedef struct {
      int  result_code;
      bool finished;
    } SignInResult;

    static SignInResult *currentResult_ = NULL;
    static NSRecursiveLock *resultLock = [NSRecursiveLock alloc];

    // ===== Keep config inputs here (since v6+ doesn't expose some props) =====
    static NSString *gServerClientID = nil;
    static NSArray<NSString *> *gRequestedScopes = nil;
    static NSString *gLoginHint = nil;

    // ===== v6+/v7: keep a global configuration object =====
    static id /* GIDConfiguration * */ gConfig = nil;

    // Read CLIENT_ID from GoogleService-Info.plist & combine with serverClientID
    static void EnsureGIDConfigurationCreated(void) {
      if (gConfig) return;

      NSString *path = [[NSBundle mainBundle] pathForResource:@"GoogleService-Info" ofType:@"plist"];
      NSDictionary *dict = [NSDictionary dictionaryWithContentsOfFile:path];
      NSString *clientId = dict[@"CLIENT_ID"];

    #if __has_include(<GoogleSignIn/GoogleSignIn.h>)
      if (clientId.length) {
        Class GIDConfigurationClass = NSClassFromString(@"GIDConfiguration");
        SEL initSel  = NSSelectorFromString(@"initWithClientID:serverClientID:");
        SEL initSel2 = NSSelectorFromString(@"initWithClientID:");
        id cfg = [GIDConfigurationClass alloc];
        if ([cfg respondsToSelector:initSel]) {
          cfg = ((id (*)(id, SEL, id, id))objc_msgSend)(cfg, initSel, clientId, gServerClientID);
        } else {
          cfg = ((id (*)(id, SEL, id))objc_msgSend)(cfg, initSel2, clientId);
        }
        gConfig = cfg;
      } else {
        NSLog(@"[GoogleSignIn] CLIENT_ID missing from GoogleService-Info.plist");
      }
    #endif
    }

    // Dummy class kept for compatibility
    @implementation GoogleSignInHandler
    @end

    // ===== Helpers =====
    static SignInResult *startSignIn() {
      bool busy = false;
      [resultLock lock];
      if (currentResult_ == NULL || currentResult_->finished) {
        if (currentResult_) { free(currentResult_); }
        currentResult_ = (SignInResult *)calloc(1, sizeof(SignInResult));
        currentResult_->result_code = 0;
        currentResult_->finished = false;
      } else {
        busy = true;
      }
      [resultLock unlock];

      if (busy) {
        NSLog(@"[GoogleSignIn] ERROR: pending sign-in already in progress.");
        SignInResult *tmp = (SignInResult *)calloc(1, sizeof(SignInResult));
        tmp->result_code = kStatusCodeDeveloperError;
        tmp->finished = true;
        return tmp; // caller must dispose
      }
      return NULL;
    }

    // ===== C API exposed to Unity =====
    extern "C" {

    // No-op (kept for API parity)
    void *GoogleSignIn_Create(void *data) { return NULL; }

    void GoogleSignIn_EnableDebugLogging(void *unused, bool flag) {
      if (flag) NSLog(@"GoogleSignIn: No optional logging available on iOS");
    }

    // Store inputs; v6+/v7 config is created later from plist + these values.
    bool GoogleSignIn_Configure(void *unused, bool useGameSignIn,
                                const char *webClientId, bool requestAuthCode,
                                bool forceTokenRefresh, bool requestEmail,
                                bool requestIdToken, bool hidePopups,
                                const char **additionalScopes, int scopeCount,
                                const char *accountName) {
      gServerClientID = webClientId ? [NSString stringWithUTF8String:webClientId] : nil;

      if (scopeCount > 0 && additionalScopes) {
        NSMutableArray *arr = [NSMutableArray arrayWithCapacity:scopeCount];
        for (int i = 0; i < scopeCount; i++) {
          if (additionalScopes[i]) [arr addObject:[NSString stringWithUTF8String:additionalScopes[i]]];
        }
        gRequestedScopes = [arr copy];
      } else {
        gRequestedScopes = nil;
      }

      gLoginHint = accountName ? [NSString stringWithUTF8String:accountName] : nil;

      return !useGameSignIn;
    }

    // Start interactive sign-in (v6/v7; runtime selector names differ)
    // Start interactive sign-in
    // Start interactive sign-in (v6/v7; runtime selector names differ)
    void *GoogleSignIn_SignIn() {
      SignInResult *result = startSignIn();
      if (!result) {
        EnsureGIDConfigurationCreated();
        UnityPause(true);

        id signIn = [GIDSignIn sharedInstance];
        UIViewController *vc = UnityGetGLViewController();

        SEL selCompletion = NSSelectorFromString(@"signInWithConfiguration:presentingViewController:completion:");
        SEL selCallback   = NSSelectorFromString(@"signInWithConfiguration:presentingViewController:callback:");

        void (^block)(id, NSError *) = ^(id user, NSError *error) {
          if (error == nil && user != nil) {
            currentResult_->result_code = kStatusCodeSuccess;
          } else {
            NSInteger code = error.code;
            NSString *domain = error.domain ?: @"";
            if (([domain containsString:@"GIDSignIn"] || [domain containsString:@"google"]) && code == -5) {
              currentResult_->result_code = kStatusCodeCanceled;
            } else {
              currentResult_->result_code = kStatusCodeError;
            }
          }
          currentResult_->finished = true;
          UnpauseUnityPlayer();
        };

        if ([signIn respondsToSelector:selCompletion]) {
          // ✅ v7 path
          ((void(*)(id,SEL,id,id,id))objc_msgSend)(signIn, selCompletion, gConfig, vc, block);
        } else if ([signIn respondsToSelector:selCallback]) {
          // fallback v6 path
          ((void(*)(id,SEL,id,id,id))objc_msgSend)(signIn, selCallback, gConfig, vc, block);
        } else {
          NSLog(@"[GoogleSignIn] ERROR: signInWithConfiguration selector not found!");
          currentResult_->result_code = kStatusCodeDeveloperError;
          currentResult_->finished = true;
        }

        result = currentResult_;
      }
      return result;
    }


    // Silent sign-in (v6/v7)
    void *GoogleSignIn_SignInSilently() {
      SignInResult *result = startSignIn();
      if (!result) {
        EnsureGIDConfigurationCreated();

        id signIn = [GIDSignIn sharedInstance];

        SEL selCompletion = NSSelectorFromString(@"restorePreviousSignInWithCompletion:");
        SEL selCallback   = NSSelectorFromString(@"restorePreviousSignInWithCallback:");

        void (^block)(id, NSError *) = ^(id user, NSError *error){
          currentResult_->result_code = (error == nil && user) ? kStatusCodeSuccess : kStatusCodeError;
          currentResult_->finished = true;
        };

        if ([signIn respondsToSelector:selCompletion]) {
          // v7
          ((void(*)(id,SEL,id))objc_msgSend)(signIn, selCompletion, block);
        } else {
          // v6
          ((void(*)(id,SEL,id))objc_msgSend)(signIn, selCallback, block);
        }

        result = currentResult_;
      }
      return result;
    }

    void GoogleSignIn_Signout() {
      [[GIDSignIn sharedInstance] signOut];
    }

    void GoogleSignIn_Disconnect() {
      id signIn = [GIDSignIn sharedInstance];
      SEL selCompletion = NSSelectorFromString(@"disconnectWithCompletion:");
      SEL selCallback   = NSSelectorFromString(@"disconnectWithCallback:");
      if ([signIn respondsToSelector:selCompletion]) {
        ((void(*)(id,SEL,id))objc_msgSend)(signIn, selCompletion, ^(NSError *e){ if(e) NSLog(@"[GID] disconnect error: %@", e); });
      } else if ([signIn respondsToSelector:selCallback]) {
        ((void(*)(id,SEL,id))objc_msgSend)(signIn, selCallback, ^(NSError *e){ if(e) NSLog(@"[GID] disconnect error: %@", e); });
      }
    }

    bool GoogleSignIn_Pending(SignInResult *result) {
      volatile bool ret;
      [resultLock lock];
      ret = !(result && result->finished);
      [resultLock unlock];
      return ret;
    }

    GIDGoogleUser *GoogleSignIn_Result(SignInResult *result) {
      if (result && result->finished) {
        return [GIDSignIn sharedInstance].currentUser;
      }
      return nullptr;
    }

    int GoogleSignIn_Status(SignInResult *result) {
      if (result) return result->result_code;
      return kStatusCodeDeveloperError;
    }

    void GoogleSignIn_DisposeFuture(SignInResult *result) {
      if (result == currentResult_) {
        free(currentResult_);
        currentResult_ = NULL;
      } else if (result) {
        free(result);
      }
    }

    // ===== Small NSString copy helper =====
    static size_t CopyNSString(NSString *src, char *dest, size_t len) {
      if (dest && src && len) {
        const char *string = [src UTF8String];
        strncpy(dest, string, len);
        return len;
      }
      return src ? ([(NSString *)src length] + 1) : 0;
    }

    // ===== Field getters =====
    size_t GoogleSignIn_GetServerAuthCode(GIDGoogleUser *guser, char *buf, size_t len) {
      if (guser && [guser respondsToSelector:@selector(serverAuthCode)]) {
        NSString *val = ((id(*)(id,SEL))objc_msgSend)(guser, @selector(serverAuthCode));
        return CopyNSString(val, buf, len);
      }
      return 0;
    }

    size_t GoogleSignIn_GetDisplayName(GIDGoogleUser *guser, char *buf, size_t len) {
      NSString *val = [guser.profile name];
      return CopyNSString(val, buf, len);
    }

    size_t GoogleSignIn_GetEmail(GIDGoogleUser *guser, char *buf, size_t len) {
      NSString *val = [guser.profile email];
      return CopyNSString(val, buf, len);
    }

    size_t GoogleSignIn_GetFamilyName(GIDGoogleUser *guser, char *buf, size_t len) {
      NSString *val = [guser.profile familyName];
      return CopyNSString(val, buf, len);
    }

    size_t GoogleSignIn_GetGivenName(GIDGoogleUser *guser, char *buf, size_t len) {
      NSString *val = [guser.profile givenName];
      return CopyNSString(val, buf, len);
    }

    size_t GoogleSignIn_GetIdToken(GIDGoogleUser *guser, char *buf, size_t len) {
      if (guser && [guser respondsToSelector:@selector(authentication)]) {
        id auth = ((id(*)(id,SEL))objc_msgSend)(guser, @selector(authentication)); // GIDAuthentication*
        if (auth && [auth respondsToSelector:@selector(idToken)]) {
          NSString *val = ((id(*)(id,SEL))objc_msgSend)(auth, @selector(idToken));
          return CopyNSString(val, buf, len);
        }
      }
      return 0;
    }

    size_t GoogleSignIn_GetImageUrl(GIDGoogleUser *guser, char *buf, size_t len) {
      NSURL *url = [guser.profile imageURLWithDimension:128];
      NSString *val = url ? [url absoluteString] : nil;
      return CopyNSString(val, buf, len);
    }

    size_t GoogleSignIn_GetUserId(GIDGoogleUser *guser, char *buf, size_t len) {
      NSString *val = [guser userID];
      return CopyNSString(val, buf, len);
    }

    } // extern "C"
