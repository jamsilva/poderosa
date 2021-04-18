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
using System.Collections.Generic;
using System.Text;
using Poderosa.Util;

namespace Poderosa.SerialPort {
    //シリアルに必要なEnum
    /// <summary>
    /// <ja>フローコントロールの設定</ja>
    /// <en>Specifies the flow control.</en>
    /// </summary>
    /// <exclude/>
    public enum FlowControl {
        /// <summary>
        /// <ja>なし</ja>
        /// <en>None</en>
        /// </summary>
#if !LIBRARY
        [EnumValue(Description = "Enum.FlowControl.None")]
#endif
        None,
        /// <summary>
        /// X ON / X OFf
        /// </summary>
#if !LIBRARY
        [EnumValue(Description = "Enum.FlowControl.Xon_Xoff")]
#endif
        Xon_Xoff,
        /// <summary>
        /// <ja>ハードウェア</ja>
        /// <en>Hardware</en>
        /// </summary>
#if !LIBRARY
        [EnumValue(Description = "Enum.FlowControl.Hardware")]
#endif
        Hardware
    }

    /// <summary>
    /// <ja>パリティの設定</ja>
    /// <en>Specifies the parity.</en>
    /// </summary>
    /// <exclude/>
    public enum Parity {
        /// <summary>
        /// <ja>なし</ja>
        /// <en>None</en>
        /// </summary>
#if !LIBRARY
        [EnumValue(Description = "Enum.Parity.NOPARITY")]
#endif
        NOPARITY = 0,
        /// <summary>
        /// <ja>奇数</ja>
        /// <en>Odd</en>
        /// </summary>
#if !LIBRARY
        [EnumValue(Description = "Enum.Parity.ODDPARITY")]
#endif
        ODDPARITY = 1,
        /// <summary>
        /// <ja>偶数</ja>
        /// <en>Even</en>
        /// </summary>
#if !LIBRARY
        [EnumValue(Description = "Enum.Parity.EVENPARITY")]
#endif
        EVENPARITY = 2
        //MARKPARITY  =        3,
        //SPACEPARITY =        4
    }

    /// <summary>
    /// <ja>ストップビットの設定</ja>
    /// <en>Specifies the stop bits.</en>
    /// </summary>
    /// <exclude/>
    public enum StopBits {
        /// <summary>
        /// <ja>1ビット</ja>
        /// <en>1 bit</en>
        /// </summary>
#if !LIBRARY
        [EnumValue(Description = "Enum.StopBits.ONESTOPBIT")]
#endif
        ONESTOPBIT = 0,
        /// <summary>
        /// <ja>1.5ビット</ja>
        /// <en>1.5 bits</en>
        /// </summary>
#if !LIBRARY
        [EnumValue(Description = "Enum.StopBits.ONE5STOPBITS")]
#endif
        ONE5STOPBITS = 1,
        /// <summary>
        /// <ja>2ビット</ja>
        /// <en>2 bits</en>
        /// </summary>
#if !LIBRARY
        [EnumValue(Description = "Enum.StopBits.TWOSTOPBITS")]
#endif
        TWOSTOPBITS = 2
    }
}
