﻿using System;
using System.Collections.Generic;

namespace Tubumu.Core.Utilities
{
    public class QueueBuffer
    {
        private Queue<ArraySegment<byte>> _segments { get; set; }

        public int Length { get; private set; }

        public void Enqueue(ArraySegment<byte> data)
        {
            _segments.Enqueue(data);
            Length += data.Count;
        }

        public ArraySegment<byte> Dequeue()
        {
            var item = _segments.Dequeue();
            if (item != null)
            {
                Length -= item.Count;
            }
            return item;
        }

        public ArraySegment<byte> Peek()
        {
            var item = _segments.Peek();
            return item;
        }

        public QueueBuffer()
        {
            _segments = new Queue<ArraySegment<byte>>();
            Length = 0;
        }
    }
}
