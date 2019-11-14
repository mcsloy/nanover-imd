using System;
using Narupa.Frontend.Input;
using UnityEngine;

namespace Narupa.Frontend.UI
{
    public interface ICursorProvider : IPosedObject, IButton
    {
        bool IsCursorActive { get; }
    }
}