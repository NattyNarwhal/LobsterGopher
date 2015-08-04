using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LobsterGopher
{
    public class LobstersItem
    {
        /*
{
  "created_at": "2015-05-22T17:30:35-03:00",
  "url": "http://dev.theladders.com/2015/05/design-principles-and-goals-being-expressive-in-code/",
  "title": "Design Principles and Goals (Part 3) - Being Expressive in Code",
  "short_id": "sndqyt",
  "score": 5,
  "comment_count": 0,
  "description": "",
  "comments_url": "https://lobste.rs/s/sndqyt/design_principles_and_goals_part_3_-_being_expressive_in_code",
  "submitter_user": {
    "username": "SeanTAllen",
    "created_at": "2012-12-01T22:10:18-04:00",
    "is_admin": false,
    "about": "http://www.monkeysnatchbanana.com",
    "is_moderator": false,
    "karma": 2621,
    "avatar_url": "https://secure.gravatar.com/avatar/3c53e91d2a6ceb1b7f202d709f638b1b?r=pg&d=mm&s=100"
  },
  "tags": [
    "programming"
  ]
}
         */
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("comments_url")]
        public string CommentsUrl { get; set; }
        [JsonProperty("short_id")]
        public string ShortId { get; set; }
        [JsonProperty("created_at")]
        public DateTime Created { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("score")]
        public int Score { get; set; }
        [JsonProperty("comments")]
        public List<LobstersComment> Comments { get; set; }
        [JsonProperty("comment_count")]
        public int CommentCount { get; set; }
        [JsonProperty("tags")]
        public List<String> Tags { get; set; }
        [JsonProperty("submitter_user")]
        public LobstersUser User { get; set; }
    }

    public class LobstersUser
    {
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("created_at")]
        public DateTime Created { get; set; }
        [JsonProperty("is_admin")]
        public bool IsAdmin { get; set; }
        [JsonProperty("is_moderator")]
        public bool IsMod { get; set; }
        [JsonProperty("about")]
        public string About { get; set; }
        [JsonProperty("karma")]
        public int Karma { get; set; }
    }

    public class LobstersComment
    {
        /*
         * created_at = 2015-05-23T10:10:22.000-05:00
updated_at = 2015-05-23T10:10:22.000-05:00
short_id = 38srko
is_deleted = false
is_moderated = false
score = 7
comment =
Worthwhile observations about game design. It’s interesting to realize it’s not really the procedural content to blame, though it’s hard to see how to make these games rewarding without some form of the gating the article doesn’t like.
url = https://lobste.rs/s/xc356n/the_problem_with_the_roguelike_metagame/comments/38srko#c_38srko
indent_level = 1
commenting_user
         */
        [JsonProperty("short_id")]
        public string ShortId { get; set; }
        [JsonProperty("comment")]
        public string Comment { get; set; }
        [JsonProperty("score")]
        public int Score { get; set; }
        [JsonProperty("is_deleted")]
        public bool IsDeleted { get; set; }
        [JsonProperty("is_moderated")]
        public bool IsModerated { get; set; }
        [JsonProperty("indent_level")]
        public int IndentLevel { get; set; }
        [JsonProperty("commenting_user")]
        public LobstersUser User { get; set; }
        [JsonProperty("created_at")]
        public DateTime Created { get; set; }
        [JsonProperty("updated_at")]
        public DateTime Updated { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class LobstersTag
    {
        /*
         * { id: 6,
    tag: 'php',
    description: 'PHP programming',
    privileged: false,
    is_media: false,
    inactive: false,
    hotness_mod: 0 },
         */
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("tag")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("privileged")]
        public bool Privileged { get; set; }
        [JsonProperty("is_media")]
        public bool Media { get; set; }
        [JsonProperty("inactive")]
        public bool Inactive { get; set; }
        [JsonProperty("hotness_mod")]
        public float HotnessModifier { get; set; }
    }
}
