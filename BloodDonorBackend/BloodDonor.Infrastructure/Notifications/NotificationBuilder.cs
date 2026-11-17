using BloodDonor.Domain.Entities;

namespace BloodDonor.Infrastructure.Notifications
{
    // Notification text comes only from these fixed templates, never from client input.
    // Messages never include a donor's name, phone number or email, and never include patient details.
    internal static class NotificationBuilder
    {
        private const int MaxMessageLength = 500;

        public static Notification Create(string recipientUserId,NotificationType type,string message,Guid? bloodRequestId,Guid? donorRequestId)
        {
            return new Notification
            {
                Id = Guid.NewGuid(),
                RecipientUserId = recipientUserId,
                Type = type,
                Message = message.Length <= MaxMessageLength ? message : message[..MaxMessageLength],
                BloodRequestId = bloodRequestId,
                DonorRequestId = donorRequestId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static string DonorRequestReceived(string hospitalName, BloodComponent component, BloodGroup bloodGroup)
        {
            return $"{hospitalName} is asking for {ComponentText(component)} ({BloodGroupText(bloodGroup)}) donors.";
        }

        public static string DonorRequestAccepted(BloodGroup bloodGroup)
        {
            return $"A donor accepted your request for {BloodGroupText(bloodGroup)}.";
        }

        public static string DonorRequestDeclined(BloodGroup bloodGroup)
        {
            return $"A donor declined your request for {BloodGroupText(bloodGroup)}.";
        }

        public static string DonorRequestCancelled(string hospitalName)
        {
            return $"{hospitalName} cancelled its request to you.";
        }

        public static string BloodRequestClosed(string hospitalName)
        {
            return $"{hospitalName} no longer needs donors for this request.";
        }

        private static string BloodGroupText(BloodGroup group)
        {
            return group switch
            {
                BloodGroup.APlus => "A+",
                BloodGroup.AMinus => "A-",
                BloodGroup.BPlus => "B+",
                BloodGroup.BMinus => "B-",
                BloodGroup.ABPlus => "AB+",
                BloodGroup.ABMinus => "AB-",
                BloodGroup.OPlus => "O+",
                BloodGroup.OMinus => "O-",
                _ => group.ToString()
            };
        }

        private static string ComponentText(BloodComponent component)
        {
            return component switch
            {
                BloodComponent.WholeBlood => "whole blood",
                BloodComponent.RedBloodCells => "red blood cells",
                BloodComponent.Plasma => "plasma",
                BloodComponent.Platelets => "platelets",
                _ => component.ToString()
            };
        }
    }
}