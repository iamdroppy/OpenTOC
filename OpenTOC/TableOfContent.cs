using System.Collections.Generic;
using System.IO;
using System.Text;
using Spectre.Console;

namespace OpenTOC
{
    internal class TableOfContent
    {
        private const int NumSnoGroups = 70;

        private TableOfContentHeader Header { get; }
        public ITableOfContentEntry[] EntryData { get; }

        private TableOfContent(TableOfContentHeader header, ITableOfContentEntry[] entryData)
        {
            Header = header;
            EntryData = entryData;
        }

        public static TableOfContent Read(BinaryReader reader)
        {
            var header = TableOfContentHeader.Read(reader);
            var groupData = new List<ITableOfContentEntry>();

            for (var i = 0; i < NumSnoGroups; i++)
            {
                if (header.EntryCounts[i] > 0)
                {
                    var currentPosition = reader.BaseStream.Position;
                    reader.BaseStream.Position = header.EntryOffsets[i] + header.SizeOf;
                    for (var j = 0; j < header.EntryCounts[i]; j++)
                    {
                        var snoGroup = reader.ReadInt32();
                        var snoId = reader.ReadInt32();
                        var pName = reader.ReadInt32();
                        var startOfStringPosition = reader.BaseStream.Position;
                        reader.BaseStream.Position = header.EntryOffsets[i] + header.SizeOf + 12 * header.EntryCounts[i] + pName;

                        StringBuilder name = new();
                        byte currentByte;
                        while ((currentByte = reader.ReadByte()) != 0)
                            name.Append((char)currentByte);
                        reader.BaseStream.Position = startOfStringPosition;
                        var nameBuilt = name.ToString();
                        groupData.Add(new TableOfContentEntry(snoGroup, snoId, pName, nameBuilt));
                    }
                    reader.BaseStream.Position = currentPosition;
                }
            }

            return new TableOfContent(header, groupData.ToArray());
        }

        private struct TableOfContentHeader
        {
            public readonly int[] EntryCounts = new int[NumSnoGroups];
            public readonly int[] EntryOffsets = new int[NumSnoGroups];
            public readonly int[] EntryUnkCounts = new int[NumSnoGroups];
            public int I0;

            /// <summary>
            /// Calculates sizeof this struct. An array is the length * sizeof(T).
            /// </summary>
            public int SizeOf => EntryCounts.Length * 4 + EntryOffsets.Length * 4 + EntryUnkCounts.Length * 4 + 4;

            public TableOfContentHeader()
            {
                EntryCounts = new int[NumSnoGroups];
                EntryOffsets = new int[NumSnoGroups];
                EntryUnkCounts = new int[NumSnoGroups];
                I0 = 0;
            }

            public static TableOfContentHeader Read(BinaryReader reader)
            {
                var header = new TableOfContentHeader();
                for (var i = 0; i < NumSnoGroups; i++) header.EntryCounts[i] = reader.ReadInt32();
                for (var i = 0; i < NumSnoGroups; i++) header.EntryOffsets[i] = reader.ReadInt32();
                for (var i = 0; i < NumSnoGroups; i++) header.EntryUnkCounts[i] = reader.ReadInt32();
                header.I0 = reader.ReadInt32();
                return header;
            }
        }

        private class TableOfContentEntry : ITableOfContentEntry
        {
            public int SnoGroup { get; }
            public int SnoId { get; }
            public int PName { get; }
            public string Name { get; }

            public TableOfContentEntry(int snoGroup, int snoId, int pName, string name)
            {
                SnoGroup = snoGroup;
                SnoId = snoId;
                PName = pName;
                Name = name;
            }
        }
    }
}