using System;
using System.Reflection;

namespace CloneExtensions
{
    class Member
    {
        public Member(MemberInfo info, Type type)
        {
            Info = info;
            Type = type;
        }

        public MemberInfo Info { get; private set; }

        public Type Type { get; private set; }
    }
}
