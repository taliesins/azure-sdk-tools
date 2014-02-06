﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Commands.Test.Utilities.HDInsight.Simulators
{
    using System;
    using Commands.Utilities.Common.Authentication;

    internal class FakeAccessToken : IAccessToken
    {
        public void AuthorizeRequest(Action<string, string> authTokenSetter)
        {
        }

        public string AccessToken { get; internal set; }
        public string UserId { get; internal set; }
        public LoginType LoginType { get; internal set; }
    }
}
