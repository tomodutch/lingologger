using System;

namespace LingoLogger.Data.Models
{
    public class Log
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}