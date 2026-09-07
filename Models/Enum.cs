using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace juggle_joy.Models
{
    public enum UserStatus
    {
        IncompleteRegistration = 0, Active = 1, Blocked = 2
    }

    public enum TaskStatus
    {
        Draft = 0, Received = 1, Ongoing = 2, Complete = 3
    }
    public enum ReadStatus
    {
        Unread = 0, Read = 1
    }

    public enum RecurringStatus
    {
        No = 0, Weekly = 1, Fortnightly = 2, Monthly = 3, Recurring = 4, Stop = 5
    }

    public enum GiftPaymentStatus
    {
        NotUsedCoupon=0,UsedCoupon=1,
    }

    public enum SubscriptionStatus
    {
        Active=1, Paused=2, Canceled=3,Resumed=4
    }

}