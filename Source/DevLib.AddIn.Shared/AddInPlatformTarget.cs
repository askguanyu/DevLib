//-----------------------------------------------------------------------
// <copyright file="AddInPlatformTarget.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.AddIn
{
    using System;

    /// <summary>
    /// Enum PlatformTargetEnum.
    /// </summary>
    public enum PlatformTargetEnum
    {
        /// <summary>
        /// Represents "/platform:anycpu".
        /// </summary>
        AnyCPU,

        /// <summary>
        /// Represents "/platform:x86".
        /// </summary>
        x86,

        /// <summary>
        /// Represents "/platform:x64".
        /// </summary>
        x64,

        /// <summary>
        /// Represents "/platform:Itanium".
        /// </summary>
        Itanium
    }

    /// <summary>
    /// Static Class AddInPlatformTarget.
    /// </summary>
    public static class AddInPlatformTarget
    {
        /// <summary>
        /// Static Method GetPlatformTargetCompilerArgument.
        /// </summary>
        /// <param name="platformTarget">Instance of PlatformTargetEnum.</param>
        /// <returns>Represents of PlatformTargetEnum string.</returns>
        public static string GetPlatformTargetCompilerArgument(PlatformTargetEnum platformTarget)
        {
            switch (platformTarget)
            {
                case PlatformTargetEnum.AnyCPU:
                    return "/platform:anycpu";
                case PlatformTargetEnum.x86:
                    return "/platform:x86";
                case PlatformTargetEnum.x64:
                    return "/platform:x64";
                case PlatformTargetEnum.Itanium:
                    return "/platform:Itanium";
            }

            throw new NotSupportedException(AddInConstants.AddInUnknownPlatformException);
        }
    }
}
