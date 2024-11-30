var path = require("path");

module.exports = {
  mode: "development",
  entry: "./src/App.fs.js",
  output: {
    path: path.join(__dirname, "./public"),
    filename: "bundle.js",
  },
  devServer: {
    static: {
      directory: path.resolve(__dirname, "public"),
      publicPath: "/",
    },
    port: 8080,
  },
  resolve: {
    modules: [path.resolve(__dirname, "node_modules")],
  },
};
