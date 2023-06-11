using System.Text;
using System;

using Cuni.NPrg038;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;

class AhoKodak {


	public static bool Matches(FileStream fs, params byte[][] patterns) {
		AhoCorasickSearch search = new AhoCorasickSearch();

		foreach (var pattern in patterns) {
			search.AddPattern(pattern);
		}
        search.Freeze();

        IByteSearchState state = search.InitialState;
        for (int i = 0; i < fs.Length; i++)
        {
			byte b =  (byte) fs.ReadByte();
            state = state.GetNextState(b);
            if (state.HasMatchedPattern)
            {
                return true;
            }
        }
        
		return false;
	}
}