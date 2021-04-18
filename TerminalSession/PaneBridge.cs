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
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Poderosa.Terminal;
using Poderosa.Sessions;
using Poderosa.View;
using Poderosa.UI;
using Poderosa.Forms;
#if !LIBRARY
using Poderosa.Commands;
#endif

namespace Poderosa.Terminal {

    internal class TerminalViewFactory : IViewFactory {
        public IPoderosaView CreateNew(IPoderosaForm parent) {
            return new TerminalView(parent, new TerminalControl());
        }

        public Type GetViewType() {
            return typeof(TerminalView);
        }
        public Type GetDocumentType() {
            return typeof(TerminalDocument);
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalSessionsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }

    //TerminalControlにビュー機能を与えるクラス
#if LIBRARY
    internal class TerminalView : IPoderosaView, IContentReplaceableViewSite {
#else
    internal class TerminalView : IPoderosaView, IContentReplaceableViewSite, IGeneralViewCommands {
#endif
        private IPoderosaForm _parent;
        private TerminalControl _control;
        private IContentReplaceableView _contentReplaceableView; //包含するやつ
#if !LIBRARY
        private IPoderosaCommand _copyCommand;
        private IPoderosaCommand _pasteCommand;
#endif

        public TerminalView(IPoderosaForm parent, TerminalControl control) {
            _parent = parent;
            _control = control;
#if !LIBRARY
            _copyCommand = TerminalSessionsPlugin.Instance.WindowManager.SelectionService.DefaultCopyCommand;
            _pasteCommand = TerminalSessionsPlugin.Instance.GetPasteCommand();
#endif
            control.Tag = this;
        }

        public TerminalControl TerminalControl {
            get {
                return _control;
            }
        }

        #region IContentReplaceableViewSite
        public IContentReplaceableView CurrentContentReplaceableView {
            get {
                return _contentReplaceableView;
            }
            set {
                _contentReplaceableView = value;
            }
        }
        #endregion

        #region IPoderosaView
        public IPoderosaDocument Document {
            get {
                return _control.CharacterDocument;
            }
        }

        public ISelection CurrentSelection {
            get {
                return _control.ITextSelection;
            }
        }

        public Control AsControl() {
            return _control;
        }
        public IPoderosaForm ParentForm {
            get {
                return _parent;
            }
        }
        #endregion

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalSessionsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

#if !LIBRARY
        #region IGeneralViewCommands
        public IPoderosaCommand Copy {
            get {
                return _copyCommand;
            }
        }

        public IPoderosaCommand Paste {
            get {
                return _pasteCommand;
            }
        }
        #endregion
#endif
    }

    //TerminalControl to TerminalView
    internal class PaneBridgeAdapter : ITypedDualDirectionalAdapterFactory<TerminalControl, TerminalView> {

        public override TerminalView GetAdapter(TerminalControl obj) {
            return ((TerminalView)obj.Tag);
        }

        public override TerminalControl GetSource(TerminalView obj) {
            return ((TerminalView)obj).TerminalControl;
        }

    }

}
