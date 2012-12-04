﻿#region License

/*
 * Copyright 2002-2012 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Globalization;
using System.Collections.Generic;

using Spring.Json;

namespace Spring.Social.Twitter.Api.Impl.Json
{
    /// <summary>
    /// JSON deserializer for Twitter status update (e.g., a "tweet").
    /// </summary>
    /// <author>Craig Walls</author>
    /// <author>Bruno Baia (.NET)</author>
    class TweetDeserializer : IJsonDeserializer
    {
        private const string TWEET_DATE_FORMAT = "ddd MMM dd HH:mm:ss zzz yyyy";

        public object Deserialize(JsonValue value, JsonMapper mapper)
        {
            Tweet tweet = new Tweet();

            tweet.ID = value.GetValue<long>("id");
            tweet.Text = value.GetValue<string>("text");
            tweet.CreatedAt = JsonUtils.ToDateTime(value.GetValueOrDefault<string>("created_at"), TWEET_DATE_FORMAT);
            JsonValue userValue = value.GetValue("user");
            if (userValue != null && userValue.IsObject)
            {
                tweet.User = mapper.Deserialize<TwitterProfile>(userValue);
                tweet.FromUser = tweet.User.ScreenName;
                tweet.FromUserId = tweet.User.ID;
                tweet.ProfileImageUrl = tweet.User.ProfileImageUrl;
            }
            tweet.ToUserId = value.GetValueOrDefault<long?>("in_reply_to_user_id");
            tweet.InReplyToUserId = value.GetValueOrDefault<long?>("in_reply_to_user_id");
            tweet.InReplyToUserScreenName = value.GetValueOrDefault<string>("in_reply_to_screen_name");
            tweet.InReplyToStatusId = value.GetValueOrDefault<long?>("in_reply_to_status_id");
            tweet.Source = value.GetValueOrDefault<string>("source");
            JsonValue placeValue = value.GetValue("place");
            if (placeValue != null && placeValue.IsObject)
            {
                tweet.Place = mapper.Deserialize<Place>(placeValue);
            }
            tweet.LanguageCode = value.GetValueOrDefault<string>("iso_language_code");
            tweet.RetweetCount = value.GetValueOrDefault<int>("retweet_count");
            JsonValue retweetedStatusValue = value.GetValue("retweeted_status");
            if (retweetedStatusValue != null && retweetedStatusValue.IsObject)
            {
                tweet.RetweetedStatus = mapper.Deserialize<Tweet>(retweetedStatusValue);
            }
            tweet.IsRetweetedByUser = value.GetValueOrDefault<bool>("retweeted");
            tweet.IsFavoritedByUser = value.GetValueOrDefault<bool>("favorited");
            JsonValue retweetIdValue = value.GetValue("current_user_retweet");
            if (retweetIdValue != null && retweetIdValue.IsObject)
            {
                tweet.RetweetIdByUser = retweetIdValue.GetValue<long?>("id");
            }

            // Entities
            JsonValue entitiesValue = value.GetValue("entities");
            if (entitiesValue != null)
            {
                tweet.Entities = new TweetEntities();
                tweet.Entities.Hashtags = DeserializeHashtags(entitiesValue.GetValue("hashtags"));
            }

            return tweet;
        }

        private IList<HashtagEntity> DeserializeHashtags(JsonValue value)
        {
            IList<HashtagEntity> hashtags = new List<HashtagEntity>();
            if (value != null)
            {
                foreach(JsonValue hashtagValue in value.GetValues())
                {
                    JsonValue indicesValues = hashtagValue.GetValue("indices");
                    if (indicesValues != null)
                    {
                        hashtags.Add(new HashtagEntity()
                        {
                            BeginOffset = indicesValues.GetValue<int>(0),
                            EndOffset = indicesValues.GetValue<int>(1),
                            Text = hashtagValue.GetValue<string>("text")
                        });
                    }
                }
            }
            return hashtags;
        }
    }
}