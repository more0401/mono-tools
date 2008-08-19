//
// Gendarme.Rules.Correctness.CallingEqualsWithNullArgRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	[Problem ("This method calls Equals(object) with a null argument.")]
	[Solution ("Pass some other appropriate argument than null, as passing null parameter should always return false.")]
	public class CallingEqualsWithNullArgRule: Rule, IMethodRule {

		// MethodSignatures.Equals check for a System.Object parameter while this rule is more general
		// and will work as long as there is a single parameter, whatever the type
		private static readonly new MethodSignature Equals = new MethodSignature ("Equals", "System.Boolean", new string [1], MethodAttributes.Public);

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Call:
				case Code.Calli:
				case Code.Callvirt:
					// if we're calling bool type.Equals({anytype})
					if (!Equals.Matches (ins.Operand as MethodReference))
						continue;

					// and that the previous, real, instruction is loading a null value
					// note: check the first parameter (not the instance)
					Instruction source = ins.TraceBack (method, -1);
					if ((source != null) && (source.OpCode.Code == Code.Ldnull))
						Runner.Report (method, ins, Severity.Low, Confidence.High);

					break;
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
