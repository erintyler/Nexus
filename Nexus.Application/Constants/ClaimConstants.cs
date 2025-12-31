namespace Nexus.Application.Constants;

/// <summary>
/// Constants for custom claim types used in the application.
/// </summary>
public static class ClaimConstants
{
    /// <summary>
    /// Claim type for Discord user ID.
    /// </summary>
    public const string DiscordId = "discord_id";

    /// <summary>
    /// Claim type for Discord username.
    /// </summary>
    public const string DiscordUsername = "discord_username";

    /// <summary>
    /// Claim type indicating claims transformation has been applied.
    /// </summary>
    public const string Transformed = "transformed";

    /// <summary>
    /// Claim type for whether user can create images.
    /// </summary>
    public const string CanCreateImage = "can_create_image";

    /// <summary>
    /// Claim type for whether user can edit images.
    /// </summary>
    public const string CanEditImage = "can_edit_image";

    /// <summary>
    /// Claim type for whether user can add comments.
    /// </summary>
    public const string CanAddComment = "can_add_comment";

    /// <summary>
    /// Claim type for whether user can add tags.
    /// </summary>
    public const string CanAddTags = "can_add_tags";

    /// <summary>
    /// Claim type for whether user can delete content.
    /// </summary>
    public const string CanDeleteContent = "can_delete_content";

    /// <summary>
    /// Claim type for whether user is an administrator.
    /// </summary>
    public const string IsAdmin = "is_admin";
}
