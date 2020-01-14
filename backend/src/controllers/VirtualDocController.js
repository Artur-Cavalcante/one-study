const Book = require("../models/Book");
const VirtualDoc = require("../models/VirtualDoc");

module.exports = {
  async allByBook(req, res) {
    const { book_id } = req.params;

    const book = await Book.findByPk(book_id, {
      include: { association: "vDocs" }
    });

    return res.json(book.vDocs);
  },

  async store(req, res) {
    const { book_id } = req.params;
    const { title, detail } = req.body;

    const book = await Book.findByPk(book_id);

    if (!book) return res.status(400).json({ error: "Book not found" });

    const vDoc = await VirtualDoc.create({
      title,
      detail,
      book_id
    });

    return res.status(201).json(vDoc);
  }
};
