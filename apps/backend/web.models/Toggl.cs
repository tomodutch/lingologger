namespace LingoLogger.Web.Models;

using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

public class TimeEntryPayload
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("workspace_id")]
    public long WorkspaceId { get; set; }

    [JsonPropertyName("project_id")]
    public long? ProjectId { get; set; }

    [JsonPropertyName("task_id")]
    public long? TaskId { get; set; }

    [JsonPropertyName("billable")]
    public bool Billable { get; set; }

    [JsonPropertyName("start")]
    public DateTimeOffset Start { get; set; }

    [JsonPropertyName("stop")]
    public DateTimeOffset? Stop { get; set; }

    [JsonPropertyName("duration")]
    public long Duration { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("tags")]
    public required List<string> Tags { get; set; }

    [JsonPropertyName("tag_ids")]
    public required object TagIds { get; set; }

    [JsonPropertyName("duronly")]
    public bool Duronly { get; set; }

    [JsonPropertyName("at")]
    public DateTime At { get; set; }

    [JsonPropertyName("server_deleted_at")]
    public DateTime? ServerDeletedAt { get; set; }

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    [JsonPropertyName("uid")]
    public long Uid { get; set; }

    [JsonPropertyName("wid")]
    public long Wid { get; set; }

    [JsonPropertyName("permissions")]
    public required List<string> Permissions { get; set; } = [];
}
