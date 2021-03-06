﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsmResolver.Net.Builder;

namespace AsmResolver.Net.Metadata
{
    public class NestedClassTable : MetadataTable<NestedClass>
    {
        public override MetadataTokenType TokenType
        {
            get { return MetadataTokenType.NestedClass; }
        }

        public override uint GetElementByteCount()
        {
            var definitionTable = TableStream.GetTable<TypeDefinition>();
            return (uint)definitionTable.IndexSize +
                   (uint)definitionTable.IndexSize;
        }

        protected override NestedClass ReadMember(MetadataToken token, ReadingContext context)
        {
            var reader = context.Reader;
            var definitionTable = TableStream.GetTable<TypeDefinition>();
           return new NestedClass(Header, token, new MetadataRow<uint, uint>()
           {
               Column1 = reader.ReadIndex(definitionTable.IndexSize),
               Column2 = reader.ReadIndex(definitionTable.IndexSize),
           });
        }

        protected override void UpdateMember(NetBuildingContext context, NestedClass member)
        {
            var row = member.MetadataRow;
            row.Column1 = member.Class.MetadataToken.Rid;
            row.Column2 = member.EnclosingClass.MetadataToken.Rid;
        }

        protected override void WriteMember(WritingContext context, NestedClass member)
        {
            var writer = context.Writer;
            var row = member.MetadataRow;
            var definitionTable = TableStream.GetTable<TypeDefinition>();

            writer.WriteIndex(definitionTable.IndexSize, row.Column1);
            writer.WriteIndex(definitionTable.IndexSize, row.Column2);
        }
    }

    public class NestedClass : MetadataMember<MetadataRow<uint, uint>>
    {
        private readonly LazyValue<TypeDefinition> _class;
        private readonly LazyValue<TypeDefinition> _enclosingClass;

        internal NestedClass(MetadataHeader header, MetadataToken token, MetadataRow<uint, uint> row)
            : base(header, token, row)
        {
            var definitionTable = header.GetStream<TableStream>().GetTable<TypeDefinition>();

            _class = new LazyValue<TypeDefinition>(() =>
            {
                TypeDefinition type;
                definitionTable.TryGetMember((int)(row.Column1 - 1), out type);
                return type;
            });

            _enclosingClass = new LazyValue<TypeDefinition>(() =>
            {
                TypeDefinition type;
                definitionTable.TryGetMember((int)(row.Column2 - 1), out type);
                return type;
            });

        }

        public TypeDefinition Class
        {
            get { return _class.Value; }
            set { _class.Value = value; }
        }

        public TypeDefinition EnclosingClass
        {
            get { return _enclosingClass.Value; }
            set { _enclosingClass.Value = value; }
        }
    }
}
