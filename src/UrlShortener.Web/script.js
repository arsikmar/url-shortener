
const apiurl = "http://localhost:8080/";

async function submit_url() {
    document.getElementById("load_svg").classList.toggle("d-none");

    var url = document.getElementById("form_text").value;
    var request = { url: url }

    console.log(url);

    const response = await fetch(apiurl, {
        method: "post",
        body: JSON.stringify(request),
        headers: {
            'Accept': 'application/json, text/plain',
            'Content-Type': 'application/json;charset=UTF-8'
        }
    })

    console.log(response)

    if (response.ok) 
    { 
        document.getElementById("load_svg").classList.toggle("d-none");   
        document.getElementById("short_url").classList.toggle("d-none");
        document.getElementById("shortened_url").value = await response.json();
    } 
    else {
        alert("Ошибка HTTP: " + response.statusText);
    }
}

function copy_url() {
    var short_url = document.getElementById("shortened_url");
    short_url.select();
    document.execCommand("copy");
    alert("URL скопирован в буфер обмена");
}