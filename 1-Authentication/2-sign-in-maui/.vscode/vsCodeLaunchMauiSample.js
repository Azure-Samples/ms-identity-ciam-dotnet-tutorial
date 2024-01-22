const {exec} = require('child_process');
const path = require("path");
exec('dotnet workload restore && dotnet run',{cwd:path.resolve('.')}, (err, output) => {
    // once the command has completed, the callback function is called
    if (err) {
        // log and return if we encounter an error
        console.error("Error on running commands: ", err)
        return
    }
    // log the output received from the command
    console.log("Output: \n", output)
}) 