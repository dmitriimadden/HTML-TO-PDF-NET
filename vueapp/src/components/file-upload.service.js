import axios from 'axios';

const BASE_URL = 'https://localhost:7075';

function upload(formData) {
    const promise = axios.postForm(BASE_URL+"/files/upload/", formData);
    return Promise.resolve(promise);
}

function getInputList()  {
    const files = axios.get(BASE_URL + "/files/GetInputFiles/");
    return Promise.resolve(files);
}
function convertFile(fileName) {
    const url = (BASE_URL + "/files/ConvertFile/" + fileName);
    const files = axios.post(url);
    return Promise.resolve(files);
}

export { upload, getInputList, convertFile }