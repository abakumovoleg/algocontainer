using System;

namespace Algo.Abstracts.Models.Messages
{
    public abstract class Message
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
    }
}