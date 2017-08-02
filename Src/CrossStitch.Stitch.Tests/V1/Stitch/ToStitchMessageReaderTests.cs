//using FluentAssertions;
//using NUnit.Framework;
//using System.IO;
//using System.Text;

//namespace CrossStitch.Stitch.Tests.V1.Stitch
//{
//    [TestFixture]
//    public class ToStitchMessageReaderTests
//    {
//        private static Stream CreateInputStream(string s)
//        {
//            var bytes = Encoding.ASCII.GetBytes(s);
//            var stream = new MemoryStream(bytes);
//            return stream;
//        }

//        [Test]
//        public void ReadMessage_Test1()
//        {
//            var input = CreateInputStream("{Id: 1}\nend\n");
//            var target = new ToStitchMessageReader(input);
//            var result = target.ReadMessage();
//            result.Should().NotBeNull();
//            result.Id.Should().Be(1);
//        }

//        [Test]
//        public void ReadMessage_Test2()
//        {
//            var input = CreateInputStream(@"
//            {
//                Id: 1
//            }
//            end
//            ");
//            var target = new ToStitchMessageReader(input);
//            var result = target.ReadMessage();
//            result.Should().NotBeNull();
//            result.Id.Should().Be(1);
//        }

//        [Test]
//        public void ReadMessage_Heartbeat()
//        {
//            var input = CreateInputStream(@"
//            {
//                Id: 1,
//                ChannelName: '_heartbeat'
//            }
//            end
//            ");
//            var target = new ToStitchMessageReader(input);
//            var result = target.ReadMessage();
//            result.Should().NotBeNull();
//            result.Id.Should().Be(1);
//            result.IsHeartbeatMessage().Should().BeTrue();
//        }
//    }
//}
