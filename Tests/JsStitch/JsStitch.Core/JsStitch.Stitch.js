const readline = require('readline');
var fs = require('fs');

// TODO: Get CorePID and monitor it

const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
    terminal: false
});
rl.setPrompt("");

var lines = [];

rl.on('line', (line) => {
    try {
        if (line == "end") {
            handleLines(lines.join(','));
            lines = [];
        }
        else {
            lines.push(line);
        }
        rl.prompt();
    } catch (e) {
        log(e);
    }
}).on('close', () => {
    log("Exiting on close event");
    //console.log('{"Command":"exiting"}');
    process.exit(0);
});

function handleLines(json) {
    var obj = JSON.parse(json);
    if (obj.ChannelName == "_heartbeat") {
        console.log('{ "Command":"sync", "Id":"' + obj.Id + '" }\nend\n');
    }
    else if (obj.ChannelName == "_exit") {
        log("Exiting on close message");
        process.exit(0);
    }
}

function log(x) {
    // Put in whatever error logging you want, here.
    //fs.appendFileSync("JsStitch.txt", x + "\n");
}

try {
    rl.prompt();
} catch (e) {
    log(e);
}