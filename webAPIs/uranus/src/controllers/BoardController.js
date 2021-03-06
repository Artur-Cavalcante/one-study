const User = require("../models/User");
const Board = require("../models/Board");

module.exports = {
  async index(req, res) {
    const { board_id } = req.params;

    const board = await Board.findByPk(board_id, {
      where: {
        user_id: req.user.id
      }
    });

    if (!board) return res.status(404).json({ error: "board not found" });

    return res.json(board);
  },
  async allByUser(req, res) {
    const user_id = req.user.id;

    const user = await User.findByPk(user_id, {
      include: {
        association: "boards",
        attributes: ["id", "title", "created_at", "updated_at"]
      }
    });

    if (!user) return res.status(404).json({ error: "user not found" });

    return res.json(user.boards);
  },

  async store(req, res) {
    const user_id = req.user.id;
    const { title } = req.body;

    const user = await User.findByPk(user_id);

    if (!user) return res.status(400).json({ error: "User not found" });

    const board = await Board.create({
      title,
      user_id
    });

    return res.status(201).json(board);
  }
};
