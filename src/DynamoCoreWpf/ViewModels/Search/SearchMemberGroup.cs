﻿using System.Collections.Generic;
using System.Linq;
using Dynamo.Core;
using Dynamo.UI;
using Dynamo.Wpf.ViewModels;

namespace Dynamo.Search
{
    public class SearchMemberGroup : NotificationObject
    {
        private List<NodeSearchElementViewModel> members;

        public string FullyQualifiedName { get; private set; }

        public string Prefix
        {
            get
            {
                if (string.IsNullOrEmpty(FullyQualifiedName))
                    return string.Empty;

                int index = FullyQualifiedName.IndexOf(delimiter);
                var name = index > -1 ? FullyQualifiedName.Substring(index + delimiter.Length)
                                      : FullyQualifiedName;

                index = name.LastIndexOf(delimiter);
                return index > -1 ? name.Substring(0, index + delimiter.Length)
                                  : string.Empty;
            }
        }

        public string GroupName
        {
            get
            {
                // Skip past the last delimiter and get the group name.
                int index = FullyQualifiedName.LastIndexOf(delimiter);
                return index > -1 ? FullyQualifiedName.Substring(index + delimiter.Length)
                                  : string.Empty;
            }
        }

        public IEnumerable<NodeSearchElementViewModel> Members
        {
            get
            {
                if (!showAllMembers)
                    return members;

                if (members.Count == 0) return members;

                //var firstMember = members[0] as NodeSearchElement;

                //// Parent items can contain 3 type of groups all together: create, action and query.
                //// We have to show only those elements, that are in the same group.
                //var siblings = firstMember.Parent.Items.OfType<BrowserInternalElement>().
                //        Where(parentNode => (parentNode as NodeSearchElement).Group == firstMember.Group);

                return members;
            }
        }

        private bool showAllMembers = false;
        private string delimiter = string.Format(" {0} ", Configurations.ShortenedCategoryDelimiter);

        internal SearchMemberGroup(string fullyQualifiedName)
        {
            FullyQualifiedName = fullyQualifiedName;
            members = new List<NodeSearchElementViewModel>();
        }

        //some UI properties which control style of one MemberGroup

        internal void AddMember(NodeSearchElementViewModel node)
        {
            members.Add(node);
        }

        public bool ContainsMember(NodeSearchElementViewModel member)
        {
            return Members.Any(m => m.Model.FullName == member.Model.FullName);
        }

        public void ExpandAllMembers()
        {
            showAllMembers = true;
            RaisePropertyChanged("Members");
        }

        public void Sort()
        {
            members = members.OrderBy(x => x.Name).ToList();
        }
    }
}