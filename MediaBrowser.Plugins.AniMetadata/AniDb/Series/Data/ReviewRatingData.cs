using System.Xml.Serialization;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data
{
    public class ReviewRatingData : RatingData
    {
        [XmlIgnore]
        public override RatingType Type => RatingType.Review;
    }
}