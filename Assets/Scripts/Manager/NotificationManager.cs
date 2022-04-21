using System;
using System.Collections.Generic;
using System.Linq;
using GameBase;
using PM.Enum.Notification;

public class NotificationManager: SingletonMonoBehaviour<NotificationManager>
{
    public void ExecuteNotification(UserNotificationInfo userNotification)
    {
        switch (userNotification.notificationType)
        {
            case NotificationType.LoginBonus:
                // ログインボーナスの場合はmessageにuserLoginBonusIdが入っている
                var userLoginBonusId = userNotification.message;
                var userLoginBonus = ApplicationContext.userData.userLoginBonusList.FirstOrDefault(u => u.id == userLoginBonusId);
                if (userLoginBonus != null) SaveDataUtil.StackableDialog.AddUserLoginBonusId(userLoginBonusId);
                break;
            default:
                break;
        }
    }
}