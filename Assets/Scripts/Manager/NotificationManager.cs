using System.Linq;
using GameBase;
using PM.Enum.Notification;

public class NotificationManager : SingletonMonoBehaviour<NotificationManager> {
    public void ExecuteNotification(UserNotificationInfo userNotification) {
        switch (userNotification.notificationType) {
            case NotificationType.NoticeEvent:
                ExecuteNotificationNoticeEvent(userNotification);
                break;
            default:
                break;
        }
    }

    private void ExecuteNotificationNoticeEvent(UserNotificationInfo userNotification) {
        switch (userNotification.notificationNoticeEventType) {
            case NotificationNoticeEventType.LoginBonus:
                // ログインボーナスの場合はmessageにuserLoginBonusIdが入っている
                var userLoginBonusId = userNotification.message;
                var userLoginBonus = ApplicationContext.userData.userLoginBonusList.FirstOrDefault(u => u.id == userLoginBonusId);
                if (userLoginBonus != null) SaveDataUtil.StackableDialog.AddUserLoginBonusId(userLoginBonusId);
                break;
            case NotificationNoticeEventType.News:
                // お知らせの場合はmessageにuserNewsIdが入っている

                break;
            default:
                break;
        }
    }
}
