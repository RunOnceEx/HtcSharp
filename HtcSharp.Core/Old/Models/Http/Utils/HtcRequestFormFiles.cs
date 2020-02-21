﻿using System.Collections.Generic;

namespace HtcSharp.Core.Old.Models.Http.Utils {
    public class HtcRequestFormFiles {
        private readonly List<HtcFile> _files;

        public HtcRequestFormFiles(IFormFileCollection files) {
            _files = new List<HtcFile>();
            foreach (var file in files) {
                _files.Add(new HtcFile(file));
            }
        }

        public HtcFile this[int index] => _files[index];

        public int Count => _files.Count;
    }
}
