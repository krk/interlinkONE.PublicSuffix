﻿
using System;
using System.Linq;

namespace PublicSuffix.Rules
{

    /// <summary>
    /// An abstract Rule class that the specific Rule Types inherit from.
    /// </summary>
    public abstract class Rule
    {
        private string _name;
        private string[] _parts;

        /// <summary>
        /// The normalized rule name
        /// </summary>
        public string Name
        {
            get { return _name; }
            protected set
            {
                _name = value;
                if (!string.IsNullOrEmpty(_name))
                {
                    _parts = _name.Split('.');
                    Array.Reverse(_parts);
                }
                else
                {
                    _parts = new string[] { };
                }
            }
        }

        /// <summary>
        /// The raw rule value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Create a new Rule instance.
        /// </summary>
        /// <param name="name">A line from a <see cref="RulesFactory" /></param>
        public Rule(string name)
        {
            this.Name = name.ToLowerInvariant();
            this.Value = this.Name;
        }

        /// <summary>
        /// An array of parts from splitting the rule along the dots.
        /// </summary>
        public string[] Parts
        {
            get { return _parts; }
        }

        /// <summary>
        /// The number of <see cref="Parts" />
        /// </summary>
        public int Length
        {
            get
            {
                return this.Parts.Length;
            }
        }

        /// <summary>
        /// Convert this rule instance to a string.
        /// </summary>
        /// <returns>The <see cref="Name" /></returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// A domain is said to match a rule if, when the domain and rule are both split, and one compares the labels from the rule to the labels from the domain, beginning at the right hand end, one finds that for every pair either they are identical, or that the label from the rule is "*" (star).
        /// The domain may legitimately have labels remaining at the end of this matching process.
        /// </summary>
        /// <param name="url">A valid url, example: http://www.google.com</param>
        /// <returns>true if the rule matches; otherwise, false.</returns>
        public virtual bool IsMatch(string url)
        {
            var host = Canonicalize(url);

            return IsMatch(host);
        }

        /// <summary>
        /// A domain is said to match a rule if, when the domain and rule are both split, and one compares the labels from the rule to the labels from the domain, beginning at the right hand end, one finds that for every pair either they are identical, or that the label from the rule is "*" (star).
        /// The domain may legitimately have labels remaining at the end of this matching process.
        /// </summary>
        /// <param name="url">A valid url, example: http://www.google.com</param>
        /// <returns>true if the rule matches; otherwise, false.</returns>
        public virtual bool IsMatch(string[] host)
        {
            var match = true;

            for (var h = 0; h < host.Length && h < this.Length; h++)
            {
                var part = this.Parts[h];

                if (part != host[h] && part != "*")
                {
                    match = false;
                    break;
                }
            }

            return match;
        }

        /// <summary>
        /// Parses a domain name from the supplied url and current <see cref="Rule" /> instance.
        /// Gets the Top, Second and Third level domains populated (if present.)
        /// </summary>
        /// <param name="url">A valid url, example: http://www.google.com</param>
        /// <returns>A valid <see cref="Domain" /> instance.</returns>
        public virtual Domain Parse(string url)
        {
            var host = Canonicalize(url);

            var domain = new Domain()
            {
                TLD = string.Join(".", host.Take(this.Length).Reverse().ToArray()),
                MainDomain = host.Skip(this.Length).FirstOrDefault(),
                SubDomain = string.Join(".", host.Skip(this.Length + 1).Reverse().ToArray())
            };

            return domain;
        }

        /// <summary>
        /// Parses a domain name from the supplied url and current <see cref="Rule" /> instance.
        /// Gets the Top, Second and Third level domains populated (if present.)
        /// </summary>
        /// <param name="url">A valid url, example: http://www.google.com</param>
        /// <returns>A valid <see cref="Domain" /> instance.</returns>
        public virtual Domain Parse(string[] host)
        {
            var domain = new Domain()
            {
                TLD = string.Join(".", host.Take(this.Length).Reverse().ToArray()),
                MainDomain = host.Skip(this.Length).FirstOrDefault(),
                SubDomain = string.Join(".", host.Skip(this.Length + 1).Reverse().ToArray())
            };

            return domain;
        }

        /// <summary>
        /// Converts a valid uri to a canonicalized uri, example: com.google.maps
        /// </summary>
        /// <param name="url">A valid url, example: http://www.google.com</param>
        /// <returns>A string array in reverse order.</returns>
        public static string[] Canonicalize(string url)
        {
            var uri = new Uri(url);
            return uri.DnsSafeHost.Split('.').Reverse().ToArray();
        }
    }
}
