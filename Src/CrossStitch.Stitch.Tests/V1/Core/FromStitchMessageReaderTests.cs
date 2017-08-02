//using FluentAssertions;
//using NUnit.Framework;
//using System.IO;
//using System.Text;
//using System.Threading;

//namespace CrossStitch.Stitch.Tests.V1.Core
//{
//    [TestFixture]
//    public class FromStitchMessageReaderTests
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
//            var input = CreateInputStream("{Id: 1, Command: 'Ack'}\nend\n");
//            var target = new FromStitchMessageReader(input);
//            var result = target.ReadMessage();
//            result.Should().NotBeNull();
//            result.Id.Should().Be(1);
//        }

//        [Test]
//        public void ReadMessage_Test2()
//        {
//            var input = CreateInputStream(@"
//            {
//                Id: 1,
//                Command: 'Ack'
//            }
//            end
//            ");
//            var target = new FromStitchMessageReader(input);
//            var result = target.ReadMessage();
//            result.Should().NotBeNull();
//            result.Id.Should().Be(1);
//            result.Command.Should().Be("Ack");
//        }
//    }
//}
