#include <glad/glad.h> 
#include <tiny_gltf.h>
#include <memory>

#include <glm/glm.hpp>
#include <glm/gtc/matrix_transform.hpp>
#include <glm/common.hpp>
#include <glm/gtc/quaternion.hpp>

class GltfLoader{
private:
    tinygltf::Model model;
public:
    GltfLoader(const std::string& path)
    {
        tinygltf::TinyGLTF loader;
        std::string err;
        std::string warn;
        loader.LoadASCIIFromFile(&model, &err, &warn, path);
    }

    std::shared_ptr<std::vector<GLuint>> buildBuffers(const tinygltf::Model& model)
    {
        auto buffers = std::make_shared<std::vector<GLuint>>(model.buffers.size(), 0);
        glGenBuffers(buffers->size(), buffers->data());

         for (auto i = 0; i < model.buffers.size(); ++i) {
            glBindBuffer(GL_ARRAY_BUFFER, buffers->at(i));
            glBufferData(GL_ARRAY_BUFFER, 
                                model.buffers[i].data.size(),
                                model.buffers[i].data.data(), GL_STATIC_DRAW);
        }
        glBindBuffer(GL_ARRAY_BUFFER, 0);
        return buffers;

    }

    std::shared_ptr<std::vector<GLuint>> buildTextures(const tinygltf::Model &model) 
    {
        auto textures = std::make_shared<std::vector<GLuint>>(model.textures.size());
        glGenTextures(textures->size(), textures->data());
        for (auto i = 0; i < textures->size(); ++i) {
            glBindTexture(GL_TEXTURE_2D, textures->at(i));
            const auto &texture = model.textures[i];
            const auto &image = model.images[texture.source];
            auto minFilter =
                texture.sampler >= 0 && model.samplers[texture.sampler].minFilter != -1
                    ? model.samplers[texture.sampler].minFilter
                    : GL_LINEAR;
            auto magFilter =
                texture.sampler >= 0 && model.samplers[texture.sampler].magFilter != -1
                    ? model.samplers[texture.sampler].magFilter
                    : GL_LINEAR;
            auto wrapS = texture.sampler >= 0 ? model.samplers[texture.sampler].wrapS
                                            : GL_REPEAT;
            auto wrapT = texture.sampler >= 0 ? model.samplers[texture.sampler].wrapT
                                            : GL_REPEAT;
            glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, image.width, image.height,
                                0, GL_RGBA, image.pixel_type, image.image.data());
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, minFilter);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, magFilter);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, wrapS);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, wrapT);
            if (minFilter == GL_NEAREST_MIPMAP_NEAREST ||
                minFilter == GL_NEAREST_MIPMAP_LINEAR  ||
                minFilter == GL_LINEAR_MIPMAP_NEAREST  ||
                minFilter == GL_LINEAR_MIPMAP_LINEAR) {
                glGenerateMipmap(GL_TEXTURE_2D);
            }
        }
        glBindTexture(GL_TEXTURE_2D, 0);
        return textures;
    }

    std::shared_ptr<tinygltf::Scene> buildScene(
        const tinygltf::Model& model, 
        unsigned int sceneIndex,
        const std::shared_ptr<std::vector<GLuint>>& buffers,
        const std::shared_ptr<std::vector<GLuint>>& textures)
    {
        auto scene = std::make_shared<tinygltf::Scene>();
        for(auto i = 0; i < model.scenes[sceneIndex].nodes.size(); ++i)
        {
            
        }
    }


    std::shared_ptr<tinygltf::Node> buildNode(
        const tinygltf::Model& model,
        unsigned int nodeIndex,
        const std::shared_ptr<std::vector<GLuint>>& buffers,
        const std::shared_ptr<std::vector<GLuint>>& textures,
        std::shared_ptr<tinygltf::Node> parent)
    {
        auto node = std::make_shared<tinygltf::Node>(parent);
        auto nodeMatrix = model.nodes[nodeIndex].matrix;
        glm::mat4 matrix(1.0f);

        if (nodeMatrix.size() == 16) {
            matrix[0].x = nodeMatrix[0], matrix[0].y = nodeMatrix[1],
            matrix[0].z = nodeMatrix[2], matrix[0].w = nodeMatrix[3];
            matrix[1].x = nodeMatrix[4], matrix[1].y = nodeMatrix[5],
            matrix[1].z = nodeMatrix[6], matrix[1].w = nodeMatrix[7];
            matrix[2].x = nodeMatrix[8], matrix[2].y = nodeMatrix[9],
            matrix[2].z = nodeMatrix[10], matrix[2].w = nodeMatrix[11];
            matrix[3].x = nodeMatrix[12], matrix[3].y = nodeMatrix[13],
            matrix[3].z = nodeMatrix[14], matrix[3].w = nodeMatrix[15];
        } else {
            if (model.nodes[nodeIndex].translation.size() == 3) {
            glm::translate(matrix, glm::vec3(model.nodes[nodeIndex].translation[0],
                                            model.nodes[nodeIndex].translation[1],
                                            model.nodes[nodeIndex].translation[2]));
            }
            if (model.nodes[nodeIndex].rotation.size() == 4) {
            matrix *= glm::mat4_cast(glm::quat(model.nodes[nodeIndex].rotation[3],
                                                model.nodes[nodeIndex].rotation[0],
                                                model.nodes[nodeIndex].rotation[1],
                                                model.nodes[nodeIndex].rotation[2]));
            }
            if (model.nodes[nodeIndex].scale.size() == 3) {
            glm::scale(matrix, glm::vec3(model.nodes[nodeIndex].scale[0],
                                        model.nodes[nodeIndex].scale[1],
                                        model.nodes[nodeIndex].scale[2]));
            }
        }
        
        return node;
    }

};
