﻿using System.Runtime.Serialization;

namespace BMW.Rheingold.Psdz
{
    public enum BackendSignatureEto
    {
        [EnumMember(Value = "FORCE")]
        FORCE,
        [EnumMember(Value = "ALLOW")]
        ALLOW,
        [EnumMember(Value = "MUST_NOT")]
        MUST_NOT
    }
}