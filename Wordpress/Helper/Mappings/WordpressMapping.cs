using AutoMapper;
using Wordpress.Models;

namespace Wordpress.Helper.Mappings
{
    public class WordpressMapping: Profile
    {
        public WordpressMapping()
        {
            CreateMap<WordpressPost, Post>()
                .ForMember(destinationMember => destinationMember.Title, opt => opt.MapFrom(sourceMember => sourceMember.title.rendered))
                .ForMember(destinationMember => destinationMember.Content, opt => opt.MapFrom(sourceMember => sourceMember.content.rendered))
                .ForMember(destinationMember => destinationMember.ImageId, opt => opt.MapFrom(sourceMember => sourceMember.featured_media));
        }
    }
}