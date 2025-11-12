using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

namespace CrystalUnbolt
{
    public enum NotificationType { DelaySeconds, DailyTime, FreeSpinAvailable }

    [System.Serializable]
    public class LocalNotificationData
    {
        public string title;
        public string message;

        [Header("Notification Type")]
        public NotificationType type = NotificationType.DelaySeconds;

        [Header("Delay (Seconds)")]
        public int delaySeconds = 10;

        [Header("Daily Time (24-hour format)")]
        public int hour = 9;
        public int minute = 0;
    }

    public class CrystalLocalNotificationManager : MonoBehaviour
    {
        [Header("Notification List")]
        public List<LocalNotificationData> notifications = new List<LocalNotificationData>();

        [Header("Random Screw Game Messages")]
        [SerializeField] private string[] morningMessages = {
            "Good morning! Your daily challenge is ready!",
            "Start your day with a fresh brain teaser!",
            "Morning energy recharged! Time to sharpen your mind!",
            "New day, new levels! Can you master them all?",
            "Rise and shine! New levels unlocked!",
            "Perfect morning for a logic challenge!",
            "Wake up your brain with strategic thinking!",
            "Fresh morning, fresh challenges await!",
            "Ready to tackle today's levels?",
            "Good morning! Your strategic skills await!"
        };

        [SerializeField] private string[] afternoonMessages = {
            "Afternoon break? Solve a quick challenge!",
            "Lunch break over? Let's boost that brain power!",
            "Midday energy boost! Try strategic thinking!",
            "Afternoon challenge time! Your skills are waiting!",
            "Break time! Ready for some strategic fun?",
            "Afternoon slump? Wake up with a brain teaser!",
            "Lunch digested? Time for mental exercise!",
            "Afternoon productivity boost awaits!",
            "Midday challenge time! Test your logic!",
            "Afternoon break perfect for quick thinking!"
        };

        [SerializeField] private string[] eveningMessages = {
            "Evening wind-down? Sharpen your mind!",
            "Relax and strategize with challenging levels!",
            "End your day with satisfying victories!",
            "Evening session! Perfect stress relief!",
            "Night time is brain time! Ready to solve?",
            "Evening relaxation with strategic thinking!",
            "Unwind from work with mental challenges!",
            "Night owl? Perfect time for logic games!",
            "Evening chill time - boost your brain!",
            "End your day on a high note!"
        };

        [SerializeField] private string[] morningTitles = {
            "Morning Puzzle Time!",
            "Good Morning, Puzzle Master!",
            "Rise and Extract Crystals!",
            "Morning Challenge!",
            "New Day, New Puzzles!",
            "Start Your Day Right!",
            "Morning Brain Boost!",
            "Fresh Puzzle Energy!",
            "Wake Up & Solve!",
            "Morning Puzzle Power!"
        };

        [SerializeField] private string[] afternoonTitles = {
            "Afternoon Break!",
            "Midday Puzzle Time!",
            "Lunch Break Challenge!",
            "Afternoon Energy!",
            "Break Time Puzzles!",
            "Afternoon Brain Boost!",
            "Midday Challenge!",
            "Lunch Break Gaming!",
            "Afternoon Puzzle Session!",
            "Midday Mental Workout!"
        };

        [SerializeField] private string[] eveningTitles = {
            "Evening Relaxation!",
            "Night Puzzle Session!",
            "End Day Challenge!",
            "Evening Wind Down!",
            "Night Time Puzzles!",
            "Evening Chill Time!",
            "Night Owl Gaming!",
            "Evening Brain Games!",
            "Night Puzzle Relaxation!",
            "End Day Puzzle Time!"
        };

        private void Start()
        {
            // Clear existing notifications first
            ClearAllNotifications();
            
            // Schedule ONLY the 3 daily notifications with random messages
            ScheduleDailyRandomNotifications();
        }

        private void ScheduleDailyRandomNotifications()
        {
            // Morning notification (9:00 AM)
            ScheduleRandomNotification("morning", 9, 0);
            
            // Afternoon notification (2:00 PM)
            ScheduleRandomNotification("afternoon", 14, 0);
            
            // Evening notification (8:00 PM)
            ScheduleRandomNotification("evening", 20, 0);
        }

        private void ScheduleRandomNotification(string timeOfDay, int hour, int minute)
        {
            string title = "";
            string message = "";

            // Use day-based seed for consistent daily randomization
            int daySeed = System.DateTime.Now.DayOfYear;
            Random.InitState(daySeed + (int)timeOfDay.GetHashCode());

            switch (timeOfDay)
            {
                case "morning":
                    title = morningTitles[Random.Range(0, morningTitles.Length)];
                    message = morningMessages[Random.Range(0, morningMessages.Length)];
                    break;
                case "afternoon":
                    title = afternoonTitles[Random.Range(0, afternoonTitles.Length)];
                    message = afternoonMessages[Random.Range(0, afternoonMessages.Length)];
                    break;
                case "evening":
                    title = eveningTitles[Random.Range(0, eveningTitles.Length)];
                    message = eveningMessages[Random.Range(0, eveningMessages.Length)];
                    break;
            }

            Debug.Log($"[LocalNotification] Scheduled {timeOfDay} notification: '{title}' - '{message}'");
            ScheduleDailyNotification(title, message, hour, minute);
        }

        public void ScheduleDailyNotification(string title, string message, int hour, int minute)
        {
            string uniqueId = $"daily_{title}_{hour}_{minute}";
            
            // Cancel any existing notification with the same identifier
            CancelNotification(uniqueId);
            
#if UNITY_ANDROID
            var channel = new AndroidNotificationChannel
            {
                Id = "default_channel",
                Name = "Game Notifications",
                Importance = Importance.High,
                Description = "Game reminder notifications",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);

            var notification = new AndroidNotification
            {
                Title = title,
                Text = message,
                SmallIcon = "icon_0",
                LargeIcon = "icon_1"
            };

            var now = System.DateTime.Now;
            var fireTime = new System.DateTime(now.Year, now.Month, now.Day, hour, minute, 0);
            if (fireTime <= now) fireTime = fireTime.AddDays(1);
            
            notification.FireTime = fireTime;
            notification.RepeatInterval = System.TimeSpan.FromDays(1);

            AndroidNotificationCenter.SendNotification(notification, "default_channel");

#elif UNITY_IOS
            var trigger = new iOSNotificationCalendarTrigger
            {
                Hour = hour,
                Minute = minute,
                Repeats = true
            };
            
            var notification = new iOSNotification
            {
                Identifier = uniqueId,
                Title = title,
                Body = message,
                ShowInForeground = true,
                ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
                Trigger = trigger
            };
            
            iOSNotificationCenter.ScheduleNotification(notification);
#endif
        }

        public void ClearAllNotifications()
        {
#if UNITY_ANDROID
            AndroidNotificationCenter.CancelAllScheduledNotifications();
#elif UNITY_IOS
            iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
        }

        public void CancelNotification(string identifier)
        {
#if UNITY_ANDROID
            // For Android, we'll cancel all and reschedule - this prevents duplicates
            AndroidNotificationCenter.CancelAllScheduledNotifications();
#elif UNITY_IOS
            iOSNotificationCenter.RemoveScheduledNotification(identifier);
#endif
        }

        public void ScheduleFreeSpinNotification()
        {
            string[] titles = {
                "Free Spin Ready!",
                "Daily Spin Unlocked!",
                "Lucky Spin Available!",
                "Free Wheel Spin!",
                "Daily Bonus Ready!"
            };
            
            string[] messages = {
                "Your daily free spin is ready! Come claim your reward!",
                "Free spin unlocked! Try your luck on the wheel!",
                "Daily spin available! Don't miss your chance!",
                "Lucky wheel is ready! Spin for amazing rewards!",
                "Free spin waiting for you! Claim it now!"
            };
            
            string title = titles[Random.Range(0, titles.Length)];
            string message = messages[Random.Range(0, messages.Length)];
            string uniqueId = "free_spin_available";
            
            // Cancel any existing free spin notification
            CancelNotification(uniqueId);
            
            // Schedule immediate notification for free spin
            ScheduleImmediateNotification(title, message, uniqueId);
        }

        public void ScheduleFreeSpinNotificationAfterCooldown(int hours, int minutes, int seconds)
        {
            string[] titles = {
                "Free Spin Ready!",
                "Daily Spin Unlocked!",
                "Lucky Spin Available!",
                "Free Wheel Spin!",
                "Daily Bonus Ready!"
            };
            
            string[] messages = {
                "Your daily free spin is ready! Come claim your reward!",
                "Free spin unlocked! Try your luck on the wheel!",
                "Daily spin available! Don't miss your chance!",
                "Lucky wheel is ready! Spin for amazing rewards!",
                "Free spin waiting for you! Claim it now!"
            };
            
            string title = titles[Random.Range(0, titles.Length)];
            string message = messages[Random.Range(0, messages.Length)];
            string uniqueId = "free_spin_cooldown";
            
            // Cancel any existing free spin cooldown notification
            CancelNotification(uniqueId);
            
            // Calculate total seconds for cooldown
            int totalSeconds = (hours * 3600) + (minutes * 60) + seconds;
            
            Debug.Log($"[LocalNotification] Scheduling free spin notification after {hours}h {minutes}m {seconds}s (total: {totalSeconds} seconds)");
            
            ScheduleDelayedNotification(title, message, uniqueId, totalSeconds);
        }

        public void ScheduleDelayedNotification(string title, string message, string identifier, int delaySeconds)
        {
#if UNITY_ANDROID
            var channel = new AndroidNotificationChannel
            {
                Id = "default_channel",
                Name = "Game Notifications",
                Importance = Importance.High,
                Description = "Game reminder notifications",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);

            var notification = new AndroidNotification
            {
                Title = title,
                Text = message,
                SmallIcon = "icon_0",
                LargeIcon = "icon_1",
                FireTime = System.DateTime.Now.AddSeconds(delaySeconds)
            };

            AndroidNotificationCenter.SendNotification(notification, "default_channel");
            Debug.Log($"[Android] Scheduled notification '{title}' to fire at {notification.FireTime}");

#elif UNITY_IOS
            var trigger = new iOSNotificationTimeIntervalTrigger
            {
                TimeInterval = new System.TimeSpan(0, 0, delaySeconds),
                Repeats = false
            };
            
            var notification = new iOSNotification
            {
                Identifier = identifier,
                Title = title,
                Body = message,
                ShowInForeground = true,
                ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
                Trigger = trigger
            };
            
            iOSNotificationCenter.ScheduleNotification(notification);
            Debug.Log($"[iOS] Scheduled notification '{title}' to fire in {delaySeconds} seconds");
#endif
        }

        public void ScheduleImmediateNotification(string title, string message, string identifier)
        {
#if UNITY_ANDROID
            var channel = new AndroidNotificationChannel
            {
                Id = "default_channel",
                Name = "Game Notifications",
                Importance = Importance.High,
                Description = "Game reminder notifications",
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);

            var notification = new AndroidNotification
            {
                Title = title,
                Text = message,
                SmallIcon = "icon_0",
                LargeIcon = "icon_1",
                FireTime = System.DateTime.Now.AddSeconds(1) // Send after 1 second
            };

            AndroidNotificationCenter.SendNotification(notification, "default_channel");

#elif UNITY_IOS
            var trigger = new iOSNotificationTimeIntervalTrigger
            {
                TimeInterval = new System.TimeSpan(0, 0, 1), // 1 second delay
                Repeats = false
            };
            
            var notification = new iOSNotification
            {
                Identifier = identifier,
                Title = title,
                Body = message,
                ShowInForeground = true,
                ForegroundPresentationOption = PresentationOption.Alert | PresentationOption.Sound,
                Trigger = trigger
            };
            
            iOSNotificationCenter.ScheduleNotification(notification);
#endif
        }

    }
}