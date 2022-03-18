using JJsUSF4Library.FileClasses.SubfileClasses;
using System;
using System.Collections.Generic;

namespace JJsUSF4Library.FileClasses
{
    public class EME : USF4File //TODO - Still being read as "Other File" at the moment
    {
        public List<Effect> Effects;

        public EME()
        {
            throw new NotImplementedException("EME should currently be read as OtherFile.");
        }
    }
}