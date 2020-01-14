FROM node:alpine

WORKDIR /app

COPY . ./

RUN yarn

RUN yarn test

CMD ["yarn", "dev"]
