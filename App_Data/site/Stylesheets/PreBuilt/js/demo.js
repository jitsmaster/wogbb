/*
 * Bootstrap Image Gallery JS Demo 3.0.0
 * https://github.com/blueimp/Bootstrap-Image-Gallery
 *
 * Copyright 2013, Sebastian Tschan
 * https://blueimp.net
 *
 * Licensed under the MIT license:
 * http://www.opensource.org/licenses/MIT
 */

/*jslint unparam: true */
/*global window, document, blueimp, $ */

var FLICKER_API_KEY = '57216d9128fb88e429afd06249ac7676';

function getAlbums() {


	$.ajax({
		url: (window.location.protocol === 'https:' ?
                'https://secure' : 'https://api') +
                '.flickr.com/services/rest/',
		data: {
			format: 'json',
			method: 'flickr.photosets.getList',
			user_id: '125207475@N08',
			api_key: FLICKER_API_KEY
		},
		dataType: 'jsonp',
		jsonp: 'jsoncallback'
	}).done(function (result) {
	    var albumsContainer = $('#albums');
	    var firstAlumId = "";
	    var firstAlumLink = null;
	    $.each(result.photosets.photoset, function (index, photoset) {
	        if (!firstAlumId) 
	            firstAlumId = photoset.id;
			var link = $('<span/>')
				.addClass('albumLink')
                .html(photoset.title._content)                
                .prop('title', 'Get Album: ' + photoset.title._content)
				.bind('click', photoset.id, function (evt) {
					$('.albumLink')
						.removeClass('albumLinkOn');
					$(this).addClass('albumLinkOn');
					getPhotos(evt.data);
				})
                .bind('onselectstart', photoset.id, function (evt) {
                    return false;
                })
                .appendTo(albumsContainer);
			if (!firstAlumLink)
			    firstAlumLink = link;
		});

	    if (firstAlumId) {
	        firstAlumLink.addClass('albumLinkOn');
	        getPhotos(firstAlumId);
	    }
	});
}

function getPhotos(albumId) {
    'use strict';

    // Load demo images from flickr:
    $.ajax({
        url: (window.location.protocol === 'https:' ?
                'https://secure' : 'https://api') +
                '.flickr.com/services/rest/',
        data: {
            format: 'json',
            method: 'flickr.photosets.getPhotos',
            photoset_id: albumId, //'72157644606964077 - WOGBB',
            api_key: FLICKER_API_KEY
        },
        dataType: 'jsonp',
        jsonp: 'jsoncallback'
    }).done(function (result) {
        var linksContainer = $('#links'),
            baseUrl;

        linksContainer.empty();

        // Add the demo images as links with thumbnails to the page:
        $.each(result.photoset.photo, function (index, photo) {
            baseUrl = 'http://farm' + photo.farm + '.static.flickr.com/' +
                photo.server + '/' + photo.id + '_' + photo.secret;
            $('<a/>')
                .append($('<img>').prop('src', baseUrl + '_s.jpg'))
                .prop('href', baseUrl + '_b.jpg')
                .prop('title', photo.title)
                .attr('data-gallery', '')
                .appendTo(linksContainer);
        });
    });
};

$(getAlbums());
