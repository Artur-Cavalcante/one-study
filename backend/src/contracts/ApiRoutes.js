const version = "/v1"

const ApiRoutes =  {
    users: {
        all: version + "/users",
        index: version + "/users/:user_id",
        create: version + "/users"
      },
      
      books: {
        allByUser: version + "/books",
        index: version + "/books/:user_id",
        create: version + "/books",
        allVirtualDoc: version + "/books/:book_id/virtual_doc"
      },
      
      virtual_doc: {
        create: version + "/books/:book_id/virtual_doc"
      },

      boards: {
          create: version + "/boards",
          allByUser: version + "/boards",
          index: version + "/boards/:user_id"
      },

      tasks: {
        create: version + "/boards/:board_id/tasks",
        allByBoard: version + "/boards/:board_id/tasks"
      }
}

export default ApiRoutes;