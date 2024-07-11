
const apiurl = "";

async function submit_url() {
    var url = document.getElementById("form_text").value;

    console.log(url);

    const response = await fetch(apiurl, {
        method: "post"
    })

    if (response.ok) 
    { 
        document.getElementById("load_svg").classList.toggle("d-none");   
        document.getElementById("short_url").classList.toggle("d-none");
        document.getElementById("shortened_url").value = await response.json();
    } 
    else {
        alert("Ошибка HTTP: " + response.status);
    }
}