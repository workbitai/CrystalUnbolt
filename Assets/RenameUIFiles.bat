@echo off
echo ==========================================
echo Unity UI Files Rename Script
echo ==========================================
echo.
echo IMPORTANT: Make sure Unity is CLOSED!
echo Press any key to continue or Ctrl+C to cancel...
pause >nul

cd /d "D:\GitProjects\CystalUnbolt\Assets\CrystalUnbolt\CrystalCrystalUnbolt\CrystalUnBoltGame\Game\Scripts\UI"

echo.
echo Current directory:
cd
echo.

echo Renaming .cs files...
echo.

if exist "UIComplete.cs" (
    ren "UIComplete.cs" "CrystalUIComplete.cs"
    ren "UIComplete.cs.meta" "CrystalUIComplete.cs.meta"
    echo [OK] UIComplete.cs -^> CrystalUIComplete.cs
) else (
    echo [SKIP] UIComplete.cs already renamed
)

if exist "UIDailySpin.cs" (
    ren "UIDailySpin.cs" "CrystalUIDailySpin.cs"
    ren "UIDailySpin.cs.meta" "CrystalUIDailySpin.cs.meta"
    echo [OK] UIDailySpin.cs -^> CrystalUIDailySpin.cs
) else (
    echo [SKIP] UIDailySpin.cs already renamed
)

if exist "UIGame.cs" (
    ren "UIGame.cs" "CrystalUIGame.cs"
    ren "UIGame.cs.meta" "CrystalUIGame.cs.meta"
    echo [OK] UIGame.cs -^> CrystalUIGame.cs
) else (
    echo [SKIP] UIGame.cs already renamed
)

if exist "UIGameOver.cs" (
    ren "UIGameOver.cs" "CrystalUIGameOver.cs"
    ren "UIGameOver.cs.meta" "CrystalUIGameOver.cs.meta"
    echo [OK] UIGameOver.cs -^> CrystalUIGameOver.cs
) else (
    echo [SKIP] UIGameOver.cs already renamed
)

if exist "UILeaderBoard.cs" (
    ren "UILeaderBoard.cs" "CrystalUILeaderBoard.cs"
    ren "UILeaderBoard.cs.meta" "CrystalUILeaderBoard.cs.meta"
    echo [OK] UILeaderBoard.cs -^> CrystalUILeaderBoard.cs
) else (
    echo [SKIP] UILeaderBoard.cs already renamed
)

if exist "UIMainMenu.cs" (
    ren "UIMainMenu.cs" "CrystalUIMainMenu.cs"
    ren "UIMainMenu.cs.meta" "CrystalUIMainMenu.cs.meta"
    echo [OK] UIMainMenu.cs -^> CrystalUIMainMenu.cs
) else (
    echo [SKIP] UIMainMenu.cs already renamed
)

if exist "UIMainMenuButton.cs" (
    ren "UIMainMenuButton.cs" "CrystalUIMainMenuButton.cs"
    ren "UIMainMenuButton.cs.meta" "CrystalUIMainMenuButton.cs.meta"
    echo [OK] UIMainMenuButton.cs -^> CrystalUIMainMenuButton.cs
) else (
    echo [SKIP] UIMainMenuButton.cs already renamed
)

if exist "UIPause.cs" (
    ren "UIPause.cs" "CrystalUIPause.cs"
    ren "UIPause.cs.meta" "CrystalUIPause.cs.meta"
    echo [OK] UIPause.cs -^> CrystalUIPause.cs
) else (
    echo [SKIP] UIPause.cs already renamed
)

if exist "UIProfilePage.cs" (
    ren "UIProfilePage.cs" "CrystalUIProfilePage.cs"
    ren "UIProfilePage.cs.meta" "CrystalUIProfilePage.cs.meta"
    echo [OK] UIProfilePage.cs -^> CrystalUIProfilePage.cs
) else (
    echo [SKIP] UIProfilePage.cs already renamed
)

if exist "UIStore.cs" (
    ren "UIStore.cs" "CrystalUIStore.cs"
    ren "UIStore.cs.meta" "CrystalUIStore.cs.meta"
    echo [OK] UIStore.cs -^> CrystalUIStore.cs
) else (
    echo [SKIP] UIStore.cs already renamed
)

echo.
echo ==========================================
echo ALL DONE! Files renamed successfully!
echo ==========================================
echo.
echo Next steps:
echo 1. Open Unity
echo 2. Wait for compile (1-2 minutes)
echo 3. Check Console - should be 0 errors!
echo.
pause





