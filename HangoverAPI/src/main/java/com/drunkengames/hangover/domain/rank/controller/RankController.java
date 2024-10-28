package com.drunkengames.hangover.domain.rank.controller;

import com.drunkengames.hangover.domain.rank.dto.RankDto;
import com.drunkengames.hangover.domain.rank.dto.RankRequestDto;
import com.drunkengames.hangover.domain.rank.service.RankService;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.media.ArraySchema;
import io.swagger.v3.oas.annotations.media.Content;
import io.swagger.v3.oas.annotations.media.Schema;
import io.swagger.v3.oas.annotations.responses.ApiResponse;
import io.swagger.v3.oas.annotations.tags.Tag;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

/**
 * <pre>
 *     랭킹 관리 컨트롤러 클래스
 * </pre>
 * @author 박봉균
 * @since JDK17 Eclipse Temurin
 */

@Tag(name = "Rank", description = "랭킹 관리")
@RestController
@RequestMapping("/rank")
@CrossOrigin("*")
@RequiredArgsConstructor
public class RankController {
    private final RankService rankService;

    @PostMapping("/update")
    @Operation(
            summary = "랭킹 등록",
            description = "닉네임, 최종 획득 자금, 최종 일차를 받아 랭킹 등록 <br>" +
                    "RankRequestDto 전달 (하단 Schemas 참고)"
    )
    public ResponseEntity<?> insertRank(
            @RequestBody @Valid RankRequestDto rankRequestDto
    ) throws Exception {
        rankService.insertRank(rankRequestDto);
        return new ResponseEntity<>("랭킹 등록 성공", HttpStatus.OK);
    }

    @GetMapping("/list")
    @Operation(
            summary = "랭킹 조회",
            description = "랭킹 top 20 조회하여 반환",
            responses = {
                @ApiResponse(
                    responseCode = "200",
                    description = "성공",
                    content = @Content(
                            array = @ArraySchema(schema = @Schema(implementation = RankDto.class)))
                )
            }
    )
    public ResponseEntity<?> listRank() throws Exception {
        return ResponseEntity.ok(rankService.listRank());
    }

}
