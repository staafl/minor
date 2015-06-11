// MIT Software License / Expat License
//
// Copyright (C) 2013 Velko Nikolov
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CoreAudioApi;

class Interop
{
    //static void Main() {
    //    Mute(false);
    //    MaxVolume();
    //}

    // http://www.codeproject.com/Articles/18520/Vista-Core-Audio-API-Master-Volume-Control 2013-03-07
    public static void Mute(bool mute)
    {
        var devEnum = new MMDeviceEnumerator();
        var endPoints = devEnum.EnumerateAudioEndPoints(EDataFlow.eRender, EDeviceState.DEVICE_STATEMASK_ALL);
        for (int ii = 0; ii < endPoints.Count; ++ii)
        {
            try
            {
                var endPoint = endPoints[ii];
                endPoint.AudioEndpointVolume.Mute = mute;
            }
            catch { }
        }
    }

    public static void MaxVolume()
    {
        var devEnum = new MMDeviceEnumerator();
        var endPoints = devEnum.EnumerateAudioEndPoints(EDataFlow.eRender, EDeviceState.DEVICE_STATEMASK_ALL);
        for (int ii = 0; ii < endPoints.Count; ++ii)
        {
            try
            {
                var endPoint = endPoints[ii];
                endPoint.AudioEndpointVolume.MasterVolumeLevelScalar = 1f;
            }
            catch { }
        }
    }

}
