FROM amazoncorretto:21-alpine3.20

# Set working directory
WORKDIR /usr/src/app

# Install necessary packages
RUN apk update && apk add --no-cache \
  graphviz curl font-noto font-noto-cjk

# Download PlantUML jar
RUN mkdir /plantuml && \
  curl -L https://github.com/plantuml/plantuml/releases/download/v1.2024.8/plantuml-asl-1.2024.8.jar -o /plantuml/plantuml.jar

# Set the default font in PlantUML configuration
RUN echo "skinparam defaultFontName NotoSans" > /plantuml/plantuml.config
