using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmGame.Dialog
{
    public record Dialog(string Id, string Text, List<DialogOption> Options);

    public record DialogOption(string Text, string NextId, bool IsEnd);
}
