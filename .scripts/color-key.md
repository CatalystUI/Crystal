### Color Key

CatalystUI build scripts use a set color key for their ANSI color output:

- **Red** (e31)
  - Error messages or critical issues
  - *\* Should override all color codes. Error messages should paint the entire line red **except for links or inline variables**.*
- **Green** (e32)
  - A resolved directory path (e.g. project dirs)
- **Yellow** (e33)
  - A relative file name which includes the file extension (e.g. project files)
- **Magenta** (e35)
  - A system executable or CLI command (similar to e36)
- **Cyan** (e36)
  - A relative file name which does not include the file extension (e.g. project names)
- **Bright Green** (e92)
  - A URL or link