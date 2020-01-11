// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit.Sdk;

namespace SixLabors.ImageSharp.Textures.Tests
{
    public class WithFileAttribute : DataAttribute
    {
        private readonly string fileName;

        public WithFileAttribute(string fileName)
        {
            this.fileName = fileName;
        }

        /// <inheritDoc />
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null) { throw new ArgumentNullException(nameof(testMethod)); }

            var path = new TestImageProvider(this.fileName, testMethod.Name, "");

            yield return new object[] { path };
        }
    }
}
