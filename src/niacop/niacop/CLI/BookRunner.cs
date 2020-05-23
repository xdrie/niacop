using niacop.Services;

namespace niacop.CLI {
    public class BookRunner : Runner<BookRunner.Options> {
        public class Options {
            
        }

        public override int run(Options options) {
            var bookInteractive = new BookInteractive();
            bookInteractive.initialize();
            bookInteractive.run();
            bookInteractive.destroy();

            return 0;
        }
    }
}