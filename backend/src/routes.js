const express = require("express");
const UserController = require("./controllers/UserController");
const BookController = require("./controllers/BookController");
const VirtualDocController = require("./controllers/VirtualDocController");
const BoardController = require("./controllers/BoardController");
const TaskController = require("./controllers/TaskController");
const ApiRoutes = require("./contracts/ApiRoutes");

const routes = express.Router();

routes.get("/", (req, res) => {
  return res.json({ hello: "world" });
});


routes.get(ApiRoutes.users.all, UserController.all);
routes.get(ApiRoutes.users.index, UserController.index);
routes.post(ApiRoutes.users.create, UserController.store);

routes.post(ApiRoutes.books.create, BookController.store);
routes.get(ApiRoutes.books.allByUser, BookController.allByUser);
routes.get(ApiRoutes.books.index, BookController.index);
routes.get(ApiRoutes.books.allVirtualDocs, VirtualDocController.allByBook);
routes.post(ApiRoutes.virtual_doc.create, VirtualDocController.store);

routes.get(ApiRoutes.boards.index, BoardController.index);
routes.get(ApiRoutes.boards.allByUser, BoardController.allByUser);
routes.post(ApiRoutes.boards.create, BoardController.store);

routes.post(ApiRoutes.tasks.store, TaskController.store);
routes.get(ApiRoutes.tasks.allByBoard, TaskController.allByBoard);

module.exports = routes;
