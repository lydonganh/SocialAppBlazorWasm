﻿namespace SocialAppLibrary.GotShared.Dtos
{
    public record NotificationDto(Guid ForUserId, string Text, DateTime When, Guid? PostId);
}
