﻿// -----------------------------------------------------------------------
// Copyright © 2012 Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel.Composition.Lightweight.Util
{
    // Extremely performance-sensitive.
    // Always safe for reading, even under concurrent writes,
    // only one writer at a time allowed.
    class SmallSparseInitonlyArray
    {
        class Element { public int Index; public object Value; }

        const int ElementsCapacity = 128;
        const int ElementIndexMask = 127;
        const int LocalOffsetMax = 3;

        Element[] _elements = null;
        SmallSparseInitonlyArray _overflow;

        public void Add(int index, object value)
        {
            if (_elements == null)
                 _elements = new Element[ElementsCapacity];

            var newElement = new Element{ Index = index, Value = value };

            var elementIndex = index & ElementIndexMask;
            var e = _elements[elementIndex];
            if (e == null)
            {
                _elements[elementIndex] = newElement;
                return;
            }
            
            if (e.Index == index)
                throw new Exception("Index already set.");

            for (int offset = 1; offset <= LocalOffsetMax; ++offset)
            {
                var nextIndex = (index + offset) & ElementIndexMask;
                e = _elements[nextIndex];
                if (e == null)
                {
                    _elements[nextIndex] = newElement;
                    return;
                }

                if (e.Index == index)
                    throw new Exception("Index already set.");
            }

            if (_overflow == null)
                _overflow = new SmallSparseInitonlyArray();

            _overflow.Add(index, value);
        }

        public bool TryGetValue(int index, out object value)
        {
            if (_elements == null)
            {
                value = null;
                return false;
            }

            var elementIndex = index & ElementIndexMask;
            var e = _elements[elementIndex];
            if (e != null && e.Index == index)
            {
                value = e.Value;
                return true;
            }

            for (int offset = 1; offset <= LocalOffsetMax; ++offset)
            {
                e = _elements[(index + offset) & ElementIndexMask];
                if (e == null)
                {
                    value = null;
                    return false;
                }

                if(e.Index == index)
                {
                    value = e.Value;
                    return true;
                }
            }

            if (_overflow != null)
                return _overflow.TryGetValue(index, out value);

            value = null;
            return false;
        }
    }
}
