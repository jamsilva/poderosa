// Copyright 2004-2017 The Poderosa Project.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

using Poderosa.Util.Collections;
#if LIBRARY
using Poderosa.Library;
#endif

namespace Poderosa.Boot {

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class TraceDocItem {
        private string _data;
        public TraceDocItem(string data) {
            _data = data;
        }
        public string Data {
            get {
                return _data;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class TraceDocument : IEnumerable<TraceDocItem> {

        private LinkedList<TraceDocItem> _items;

        public TraceDocument() {
            _items = new LinkedList<TraceDocItem>();
        }

        public bool IsEmpty {
            get {
                return _items.Count == 0;
            }
        }

        public void Append(string data) {
            _items.AddLast(new TraceDocItem(data));
#if UNITTEST || DEBUG
            Debug.WriteLine(data);
#endif
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return _items.GetEnumerator();
        }
        IEnumerator<TraceDocItem> IEnumerable<TraceDocItem>.GetEnumerator() {
            return _items.GetEnumerator();
        }
#if UNITTEST
        //期待通りのエラーメッセージが出ていることを確認するために必要
        public string GetDataAt(int index) {
            return CollectionUtil.GetItemFromLinkedList(_items, index).Data;
        }
#endif

    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ITracer {
        void Trace(string string_id);
        void Trace(string string_id, string param1);
        void Trace(string string_id, string param1, string param2);
        void Trace(Exception ex);

        TraceDocument Document {
            get;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class DefaultTracer : ITracer {
        private TraceDocument _document;
        private StringResource _strResource;

        public DefaultTracer(StringResource sr) {
            _document = new TraceDocument();
            _strResource = sr;
        }

        public TraceDocument Document {
            get {
                return _document;
            }
        }

        public void Trace(string string_id) {
            _document.Append(_strResource.GetString(string_id));
        }

        public void Trace(string string_id, string param1) {
#if LIBRARY
            _document.Append(string_id + " (" + param1 + ")");
#else
            _document.Append(String.Format(_strResource.GetString(string_id), param1));
#endif
        }

        public void Trace(string string_id, string param1, string param2) {
#if LIBRARY
            _document.Append(string_id + " (" + param1 + ", " + param2 + ")");
#else
            _document.Append(String.Format(_strResource.GetString(string_id), param1, param2));
#endif
        }

        public void Trace(Exception ex) {
            _document.Append(ex.Message);
            _document.Append(ex.StackTrace);
        }
    }
}
