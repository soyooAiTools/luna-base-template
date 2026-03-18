using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameManager.Event
{
    public abstract class GameEventArgs : EventArgs
    {
        public abstract int Id
        {
            get;
        }
    }
}
